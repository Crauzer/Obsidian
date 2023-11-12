export const wadQueryKeys = {
  mountedWads: ['mounted_wads'] as const,
  mountedWad: (wadId: string) => ['mounted_wads', wadId] as const,
  mountedWadItems: (wadId: string) => ['mounted_wads', wadId, 'items'] as const,
  mountedWadDirectoryItems: (wadId: string, itemId: string) =>
    ['mounted_wads', wadId, 'items', itemId, 'items'] as const,
  mountedWadDirectoryPathComponents: (wadId: string, itemId: string) =>
    ['mounted_wads', wadId, 'items', itemId, 'path_components'] as const,
  wadTree: (wadId: string) => ['wad', wadId, 'tree'] as const,
  wadItems: (wadId: string) => ['wad', wadId, 'items'] as const,
};
