import { z } from 'zod';

export type Settings = {
  wadHashtables: string[];
};

export type SettingsFormData = z.infer<typeof settingsFormDataSchema>;
export const settingsFormDataSchema = z.object({
  wadHashtables: z.array(z.object({ url: z.string().url().optional() })),
});
