export const actionsQueryKeys = {
  actionProgress: (actionId: string) =>
    ["actions", actionId, "progress"] as const,
};
