import { z } from 'zod';

export type Settings = z.infer<typeof settingsSchema>;
export const settingsSchema = z.object({ wadHashtables: z.array(z.string()) });
