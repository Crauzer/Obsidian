import { z } from 'zod';

import { pathStringSchema } from '../../utils';

export type Settings = {
  defaultMountDirectory: string;
  defaultExtractionDirectory: string;
};

export type SettingsFormData = z.infer<typeof settingsFormDataSchema>;
export const settingsFormDataSchema = z.object({
  defaultMountDirectory: pathStringSchema,
  defaultExtractionDirectory: pathStringSchema,
});
