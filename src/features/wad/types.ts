import { z } from 'zod';

export type MountedWadsResponse = {
  wads: MountedWad[];
};

export type MountWadResponse = {
  wadId: string;
};

export type MountedWad = {
  id: string;
  name: string;
  wadPath: string;
};

export type WadItemPathComponent = {
  name: string;
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
  kind: 'file';
  name: string;
  path: string;
  nameHash: number;
  pathHash: number;
  extensionKind: LeagueFileKind;
  isSelected: boolean;
  isChecked: boolean;
};

export type WadDirectoryItem = z.infer<typeof wadDirectoryItemSchema>;
export const wadDirectoryItemSchema = z.object({
  id: z.string().uuid(),
  kind: z.literal('directory'),
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
export const wadItemKindSchema = z.enum(['file', 'directory']);

export type LeagueFileKind =
  | 'animation'
  | 'jpeg'
  | 'lua_obj'
  | 'map_geometry'
  | 'png'
  | 'preload'
  | 'property_bin'
  | 'riot_string_table'
  | 'simple_skin'
  | 'skeleton'
  | 'static_mesh_ascii'
  | 'static_mesh_binary'
  | 'texture_dds'
  | 'texture'
  | 'unknown'
  | 'world_geometry'
  | 'wwise_bank'
  | 'wwise_package';
