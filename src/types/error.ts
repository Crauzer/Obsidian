import { z } from "zod";

export const emptyExtensionSchema = z.object({
  kind: z.literal("empty"),
});

export const wadHashtablesMissingExtensionSchema = z.object({
  kind: z.literal("wadHashtablesMissing"),
});

const apiErrorExtensions = {
  wadHashtablesMissing: wadHashtablesMissingExtensionSchema,
};

export type ApiErrorExtensionKind = keyof typeof apiErrorExtensions;

export type ApiErrorExtension = z.infer<typeof apiErrorExtensionSchema>;
const apiErrorExtensionSchema = z.discriminatedUnion("kind", [
  emptyExtensionSchema,
  wadHashtablesMissingExtensionSchema,
]);

export type ApiError = z.infer<typeof apiErrorSchema>;
export const apiErrorSchema = z.object({
  title: z.string().nullable().optional(),
  message: z.string(),
  extensions: z.array(apiErrorExtensionSchema).optional().nullable(),
});

export const getApiErrorExtension = <
  TExtension extends z.ZodObject<{ kind: z.ZodLiteral<ApiErrorExtensionKind> }>,
>(
  error: ApiError,
  extension: TExtension,
): z.infer<TExtension> | undefined => {
  return extension.parse(
    error.extensions?.find((x) => x.kind === extension.shape.kind.value),
  );
};
