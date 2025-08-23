import { z } from "zod";

export const fileDropPayloadSchema = z.array(z.string());

export const createEventSchema = <TPayload extends z.AnyZodObject>(
  payloadSchema: TPayload,
) => {
  return z.object({
    event: z.string(),
    id: z.number(),
    windowLabel: z.string().nullable(),
    payload: payloadSchema,
  });
};
