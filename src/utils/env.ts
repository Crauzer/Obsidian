import { z } from 'zod';

export type Env = z.infer<typeof envSchema>;
export const envSchema = z.object({
  MODE: z.string(),
  BASE_URL: z.string(),
  PROD: z.boolean(),
  DEV: z.boolean(),
  SSR: z.boolean(),
});

export const env = envSchema.parse(import.meta.env);
