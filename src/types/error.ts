import { z } from 'zod';

export const apiErrorSchema = z.object({
  title: z.string().optional(),
  message: z.string(),
  extensions: z.array(z.object({})),
});
