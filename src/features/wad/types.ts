import { z } from "zod";

export type MountedWadsResponse = {
  wads: MountedWad[];
};

export type MountWadResponse = {
  wadIds: string[];
};

export type ExtractMountedWadResponse = {
  actionId: string;
};

export type SearchWadResponse = {
  items: SearchWadResponseItem[];
};

export type SearchWadResponseItem = {
  id: string;
  parentId?: string;
  name: string;
  path: string;
  extensionKind: LeagueFileKind;
};

export type MountedWad = {
  id: string;
  name: string;
  wadPath: string;
};

export type WadItemPathComponent = {
  name: string;
  path: string;
  itemId: string;
};

export type VirtualizedWadTreeItem = {
  kind: WadItemKind;
  level: number;
  id: string;
  path: string;
  name: string;
  nameHash: number;
  pathHash: number;
  isSelected: boolean;
  isChecked: boolean;
  isExpanded: boolean;
};

export type WadTreeResponse = {
  id: string;
  wadPath: string;
  items: WadItem[];
};

export type WadItemsPage = {
  cursor: number;
  nextCursor: number;
  prevCursor: number;

  items: WadItem[];
};

export type WadFileItem = {
  id: string;
  kind: "file";
  name: string;
  path: string;
  nameHash: number;
  pathHash: number;

  compressionKind: WadChunkCompressionKind;
  compressedSize: number;
  uncompressedSize: number;

  extensionKind: LeagueFileKind;
  isSelected: boolean;
  isChecked: boolean;
};

export type WadDirectoryItem = z.infer<typeof wadDirectoryItemSchema>;
export const wadDirectoryItemSchema = z.object({
  id: z.string().uuid(),
  kind: z.literal("directory"),
  name: z.string(),
  path: z.string(),
  nameHash: z.number(),
  pathHash: z.number(),
  isSelected: z.boolean(),
  isChecked: z.boolean(),
  isExpanded: z.boolean(),
});

export type WadItem = WadFileItem | WadDirectoryItem;

export type WadItemKind = z.infer<typeof wadItemKindSchema>;
export const wadItemKindSchema = z.enum(["file", "directory"]);

export type WadItemSelectionUpdate = {
  index: number;
  isSelected: boolean;
};

export type LeagueFileKind =
  | "animation"
  | "jpeg"
  | "light_grid"
  | "lua_obj"
  | "map_geometry"
  | "png"
  | "preload"
  | "property_bin"
  | "property_bin_override"
  | "riot_string_table"
  | "simple_skin"
  | "skeleton"
  | "static_mesh_ascii"
  | "static_mesh_binary"
  | "svg"
  | "texture"
  | "texture_dds"
  | "unknown"
  | "world_geometry"
  | "wwise_bank"
  | "wwise_package";

export type WadChunkCompressionKind =
  | "raw"
  | "gzip"
  | "file_redirect"
  | "zstd"
  | "zstd_multi";

export type WadChunkPreviewType = "image";
