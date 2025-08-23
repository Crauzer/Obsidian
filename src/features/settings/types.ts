import { z } from "zod";

import { pathStringSchema } from "../../utils";

export type Settings = {
  openDirectoryAfterExtraction: boolean;
  defaultMountDirectory: string | null;
  defaultExtractionDirectory: string | null;
};

export type SettingsFormData = z.infer<typeof settingsFormDataSchema>;
export const settingsFormDataSchema = z.object({
  openDirectoryAfterExtraction: z.boolean(),
  defaultMountDirectory: pathStringSchema.nullable(),
  defaultExtractionDirectory: pathStringSchema.nullable(),
});
