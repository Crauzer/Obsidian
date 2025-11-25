import clsx from "clsx";
import type React from "react";
import { useCallback, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import {
  FiChevronRight,
  FiFolder,
  FiHardDrive,
  FiSearch,
} from "react-icons/fi";
import AutoSizer from "react-virtualized-auto-sizer";
import { Virtuoso } from "react-virtuoso";

import { FolderIcon } from "../../../assets";
import { Icon, Input, Spinner } from "../../../components";
import { appRoutes } from "../../../lib/router";
import {
  getLeagueFileKindIcon,
  getLeagueFileKindIconColor,
} from "../../wad/utils";
import {
  useGameExplorerItems,
  useGameExplorerPathComponents,
  useGameExplorerStatus,
  useMountGameExplorer,
} from "../api";
import type { GameExplorerItem, GameExplorerPathComponent } from "../types";

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return "0 B";
  const k = 1024;
  const sizes = ["B", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${Number.parseFloat((bytes / k ** i).toFixed(1))} ${sizes[i]}`;
};

export const GameExplorer: React.FC = () => {
  const [t] = useTranslation("gameExplorer");
  const [searchQuery, setSearchQuery] = useState("");
  const [currentParentId, setCurrentParentId] = useState<string | undefined>(
    undefined,
  );

  // Step 1: Check status (is league directory configured?)
  const statusQuery = useGameExplorerStatus();
  const hasLeagueDirectory = !!statusQuery.data?.basePath;
  const isAlreadyInitialized = statusQuery.data?.isInitialized ?? false;

  // Step 2: Mount game explorer (runs automatically when league dir is configured and not yet initialized)
  const mountQuery = useMountGameExplorer(
    hasLeagueDirectory && !isAlreadyInitialized,
  );

  // The explorer is ready when either already initialized OR mount query succeeded
  const isReady = isAlreadyInitialized || mountQuery.isSuccess;

  // Step 3: Get items (only when ready)
  const itemsQuery = useGameExplorerItems(currentParentId, isReady);
  const pathComponentsQuery = useGameExplorerPathComponents(
    isReady ? currentParentId : undefined,
  );

  const filteredItems = useMemo(() => {
    if (!itemsQuery.data) return [];
    if (!searchQuery.trim()) return itemsQuery.data;

    const query = searchQuery.toLowerCase();
    return itemsQuery.data.filter((item) =>
      item.name.toLowerCase().includes(query),
    );
  }, [itemsQuery.data, searchQuery]);

  const handleItemClick = useCallback((item: GameExplorerItem) => {
    if (item.kind === "directory") {
      setCurrentParentId(item.id);
      setSearchQuery("");
    }
  }, []);

  const handleBreadcrumbClick = useCallback(
    (component?: GameExplorerPathComponent) => {
      if (component) {
        setCurrentParentId(component.id);
      } else {
        setCurrentParentId(undefined);
      }
      setSearchQuery("");
    },
    [],
  );

  // Show loading state
  if (statusQuery.isLoading || mountQuery.isLoading) {
    return (
      <div className="flex h-full w-full flex-col items-center justify-center gap-4">
        <Spinner />
        <span className="text-gray-400">{t("loading")}</span>
      </div>
    );
  }

  // Show error/not configured state
  if (statusQuery.isError || !hasLeagueDirectory) {
    return (
      <div className="flex h-full w-full flex-col items-center justify-center gap-4 p-8">
        <Icon size="xl" icon={FiHardDrive} className="text-gray-500" />
        <p className="text-center text-gray-400">
          {t("noDirectoryConfigured")}
        </p>
        <a
          href={appRoutes.settings}
          className="text-obsidian-400 underline hover:text-obsidian-300"
        >
          {t("configureInSettings")}
        </a>
      </div>
    );
  }

  return (
    <div className="flex h-full w-full flex-col gap-2 p-2">
      <div className="flex h-full flex-col rounded border border-gray-600 bg-gray-900">
        {/* Header */}
        <div className="flex w-full flex-row flex-wrap items-center gap-4 border-b border-gray-600 bg-gray-800 p-3">
          <div className="flex items-center gap-2">
            <Icon size="lg" icon={FiFolder} className="text-amber-500" />
            <span className="text-lg font-semibold text-gray-100">
              {t("title")}
            </span>
          </div>

          {/* Breadcrumbs */}
          <div className="flex min-w-[300px] flex-1 items-center gap-1 rounded bg-gray-900/50 px-2 py-1">
            <button
              type="button"
              onClick={() => handleBreadcrumbClick()}
              className={clsx(
                "rounded px-2 py-0.5 text-sm transition-colors",
                !currentParentId
                  ? "bg-obsidian-600/30 text-obsidian-300"
                  : "text-gray-400 hover:bg-gray-700 hover:text-gray-200",
              )}
            >
              {t("root")}
            </button>
            {pathComponentsQuery.data?.map((component, index) => (
              <span key={component.id} className="flex items-center gap-1">
                <Icon
                  size="sm"
                  icon={FiChevronRight}
                  className="text-gray-600"
                />
                <button
                  type="button"
                  onClick={() => handleBreadcrumbClick(component)}
                  className={clsx(
                    "rounded px-2 py-0.5 text-sm transition-colors",
                    index === (pathComponentsQuery.data?.length ?? 0) - 1
                      ? "bg-obsidian-600/30 text-obsidian-300"
                      : "text-gray-400 hover:bg-gray-700 hover:text-gray-200",
                  )}
                >
                  {component.name}
                </button>
              </span>
            ))}
          </div>

          <div className="relative w-64">
            <Icon
              size="sm"
              icon={FiSearch}
              className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"
            />
            <Input
              className="pl-9"
              placeholder={t("searchPlaceholder")}
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>

          <span className="text-sm text-gray-400">
            {mountQuery.data?.wadCount ?? statusQuery.data?.wadCount ?? 0} WADs
            • {filteredItems.length} {t("itemsShown")}
          </span>
        </div>

        {/* Item List */}
        <div className="flex-1">
          {itemsQuery.isLoading ? (
            <div className="flex h-full items-center justify-center">
              <Spinner />
            </div>
          ) : (
            <AutoSizer>
              {({ height, width }) => (
                <Virtuoso
                  style={{ height, width }}
                  data={filteredItems}
                  itemContent={(_index, item) => (
                    <GameExplorerItemRow
                      key={item.id}
                      item={item}
                      onClick={() => handleItemClick(item)}
                    />
                  )}
                />
              )}
            </AutoSizer>
          )}
        </div>
      </div>
    </div>
  );
};

type GameExplorerItemRowProps = {
  item: GameExplorerItem;
  onClick: () => void;
};

const GameExplorerItemRow: React.FC<GameExplorerItemRowProps> = ({
  item,
  onClick,
}) => {
  const isDirectory = item.kind === "directory";

  return (
    <button
      type="button"
      className={clsx(
        "group flex w-full cursor-pointer select-none flex-row items-center gap-3 border border-transparent px-3 py-2 text-left",
        "transition-colors duration-150",
        "hover:border-obsidian-500/30 hover:bg-obsidian-700/20",
      )}
      onClick={onClick}
      onDoubleClick={onClick}
    >
      {/* Icon */}
      {isDirectory ? (
        <Icon size="md" className="fill-amber-500" icon={FolderIcon} />
      ) : (
        <Icon
          size="md"
          className={clsx(
            getLeagueFileKindIconColor(
              item.extensionKind as Parameters<
                typeof getLeagueFileKindIconColor
              >[0],
            ),
          )}
          icon={getLeagueFileKindIcon(
            item.extensionKind as Parameters<typeof getLeagueFileKindIcon>[0],
          )}
        />
      )}

      {/* Name and details */}
      <div className="flex flex-1 flex-col">
        <span className="font-medium text-gray-100 group-hover:text-obsidian-300">
          {item.name}
        </span>
        {!isDirectory && (
          <span className="text-xs text-gray-500">
            {item.wadName} • {formatFileSize(item.uncompressedSize)}
          </span>
        )}
        {isDirectory && (
          <span className="text-xs text-gray-500">{item.itemCount} items</span>
        )}
      </div>

      {/* Size for files */}
      {!isDirectory && (
        <div className="text-sm text-gray-400">
          {formatFileSize(item.compressedSize)}
        </div>
      )}
    </button>
  );
};
