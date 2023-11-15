export type ActionProgress = ActionProgressFinished | ActionProgressWorking;

export type ActionProgressWorking = {
  status: 'working';
  progress: number;
};

export type ActionProgressFinished = {
  status: 'finished';
};
