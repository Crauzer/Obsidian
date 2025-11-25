export type GameExplorerStatusResponse = {
  isInitialized: boolean;
  wadCount: number;
  basePath: string | null;
};

export type MountGameExplorerResponse = {
  wadCount: number;
  basePath: string;
};

export type GameExplorerFileItem = {
  kind: "file";
  id: string;
  name: string;
  path: string;
  wadId: string;
  wadName: string;
  itemId: string;
  extensionKind: string;
  compressedSize: number;
  uncompressedSize: number;
};

export type GameExplorerDirectoryItem = {
  kind: "directory";
  id: string;
  name: string;
  path: string;
  itemCount: number;
};

export type GameExplorerItem = GameExplorerFileItem | GameExplorerDirectoryItem;

export type GameExplorerPathComponent = {
  id: string;
  name: string;
  path: string;
};
