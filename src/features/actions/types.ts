import { z } from "zod";

import { createEventSchema } from "../../types";

export type ActionProgressEvent = z.infer<typeof actionProgressEventSchema>;
export const actionProgressEventSchema = createEventSchema(
  z.object({ progress: z.number(), message: z.string().optional() }),
);

export type ActionProgress = ActionProgressFinished | ActionProgressWorking;

export type ActionProgressWorking = {
  status: "working";
  progress: number;
};

export type ActionProgressFinished = {
  status: "finished";
};
