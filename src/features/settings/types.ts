import { z } from 'zod';

export type Settings = {
  defaultMountDirectory: string;
  defaultExtractionDirectory: string;
};

export type SettingsFormData = z.infer<typeof settingsFormDataSchema>;
export const settingsFormDataSchema = z.object({
  wadHashtableSources: z.array(z.object({ url: z.string().url().optional() })),
});
