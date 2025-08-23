import clsx from "clsx";
import React, { type MouseEventHandler, useCallback } from "react";

import { FolderIcon } from "../../../../assets";
import { Icon, Tooltip } from "../../../../components";
import { useWadContext } from "../../providers";
import type { WadItem } from "../../types";
import { getLeagueFileKindIcon, getLeagueFileKindIconColor } from "../../utils";
import { WadItemRowContextMenu } from "./contextMenu";

export type WadItemListRowProps = {
  wadId: string;
  parentItemId?: string;
  item: WadItem;
  index: number;
  onClick?: MouseEventHandler;
};

export const WadItemListRow = ({
  wadId,
  parentItemId,
  item,
  onClick,
}: WadItemListRowProps) => {
  return (
    <WadItemRowContextMenu
      wadId={wadId}
      parentItemId={parentItemId}
      item={item}
    >
      <WadItemListRowContent item={item} onClick={onClick} />
    </WadItemRowContextMenu>
  );
};

type WadItemListRowContentProps = {
  item: WadItem;
  onClick?: MouseEventHandler;
};

const WadItemListRowContent = ({
  item,
  onClick,
}: WadItemListRowContentProps) => {
  const { navigate } = useWadContext();

  const handleDoubleClick = useCallback(() => {
    if (item.kind !== "directory") {
      return;
    }

    navigate(item.id);
  }, [item.id, item.kind, navigate]);

  return (
    <div
      className={clsx(
        "text-md box-border flex select-none flex-row border py-1 pl-2 text-gray-50 hover:cursor-pointer",
        { "hover:bg-gray-700/25": !item.isSelected },
        {
          "border-obsidian-500/40 bg-obsidian-700/40": item.isSelected,
          "border-transparent": !item.isSelected,
        },
      )}
      onClick={(e) => onClick?.(e)}
      onDoubleClick={handleDoubleClick}
      onContextMenu={() => {}}
    >
      <Tooltip.Root>
        <Tooltip.Trigger asChild>
          <div className="flex flex-row items-center gap-2">
            {item.kind === "directory" ? (
              <Icon size="md" className="fill-amber-500" icon={FolderIcon} />
            ) : (
              <Icon
                size="md"
                className={clsx(getLeagueFileKindIconColor(item.extensionKind))}
                icon={getLeagueFileKindIcon(item.extensionKind)}
              />
            )}
            {item.name}
          </div>
        </Tooltip.Trigger>
        <Tooltip.Content side="right" className="flex flex-col gap-2 px-2 py-2">
          <span>
            <span className="font-bold">Path: </span>
            <span className="rounded-lg bg-gray-900/50 p-1">{item.path}</span>
          </span>
          {item.kind === "file" && (
            <>
              {" "}
              <span>
                <span className="font-bold">Compression: </span>
                <span className="rounded-lg bg-gray-900/50 p-1">
                  {item.compressionKind}
                </span>
              </span>
              <span>
                <span className="font-bold">Compressed Size: </span>
                <span className="rounded-lg bg-gray-900/50 p-1">
                  {item.compressedSize}
                </span>
              </span>
              <span>
                <span className="font-bold">Uncompressed Size: </span>
                <span className="rounded-lg bg-gray-900/50 p-1">
                  {item.uncompressedSize}
                </span>
              </span>
            </>
          )}
        </Tooltip.Content>
      </Tooltip.Root>
    </div>
  );
};
