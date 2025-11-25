export const gameExplorerQueryKeys = {
  status: ["game-explorer", "status"] as const,
  mount: ["game-explorer", "mount"] as const,
  items: (parentId?: string) =>
    parentId
      ? (["game-explorer", "items", parentId] as const)
      : (["game-explorer", "items"] as const),
  pathComponents: (itemId?: string) =>
    ["game-explorer", "path-components", itemId] as const,
};

