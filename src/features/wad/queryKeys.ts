export const wadQueryKeys = {
  mountedWad: (wadId: string) => ["mounted_wads", wadId] as const,
  mountedWadDirectoryPathComponents: (wadId: string, itemId: string) =>
    ["mounted_wads", wadId, "items", itemId, "path_components"] as const,
  mountedWadItems: (wadId: string) => ["mounted_wads", wadId, "items"] as const,
  mountedWads: ["mounted_wads"] as const,
  wadParentItems: (wadId: string, parentId: string | undefined) =>
    ["wad", wadId, "items", parentId, "items"] as const,
  wadTree: (wadId: string) => ["wad", wadId, "tree"] as const,
  wadSearch: (wadId: string, query: string) =>
    ["wad", wadId, "search", query] as const,

  itemPreviewTypes: (wadId: string, itemId: string) =>
    ["wad", wadId, "items", itemId, "preview_types"] as const,
  imagePreviewUrl: (wadId: string, itemId: string) =>
    ["wad", wadId, "items", itemId, "preview_url"] as const,
};
