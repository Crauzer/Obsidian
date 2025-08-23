import type React from "react";
import { useMemo, useState } from "react";
import { Panel, PanelGroup, PanelResizeHandle } from "react-resizable-panels";
import { generatePath, useNavigate } from "react-router-dom";

import { appRoutes } from "../../../../lib/router";
import { composeUrlQuery } from "../../../../utils";
import { useWadDirectoryPathComponents, useWadParentItems } from "../../api";
import { WadContext, type WadContextState } from "../../providers";
import type { WadItem, WadItemPathComponent } from "../../types";
import { WadSearchInput } from "../search";
import { WadItemList } from "../wadItemList";
import { ExtractAllButton } from "./ExtractAllButton";
import { WadBreadcrumbs } from "./WadBreadcrumbs";

export type WadRootTabContentProps = { wadId: string };

export const WadRootTabContent: React.FC<WadRootTabContentProps> = ({
  wadId,
}) => {
  const itemsQuery = useWadParentItems({ wadId, parentId: undefined });

  if (itemsQuery.isSuccess) {
    return (
      <WadTabContent
        wadId={wadId}
        parentItemId={undefined}
        items={itemsQuery.data}
        pathComponents={[]}
      />
    );
  }

  return null;
};

export type WadDirectoryTabContentProps = {
  wadId: string;
  selectedItemId: string;
};

export const WadDirectoryTabContent: React.FC<WadDirectoryTabContentProps> = ({
  wadId,
  selectedItemId,
}) => {
  const pathComponentsQuery = useWadDirectoryPathComponents({
    wadId,
    itemId: selectedItemId,
  });
  const itemsQuery = useWadParentItems({ wadId, parentId: selectedItemId });

  if (itemsQuery.isSuccess) {
    return (
      <WadTabContent
        wadId={wadId}
        parentItemId={selectedItemId}
        items={itemsQuery.data}
        pathComponents={pathComponentsQuery.data ?? []}
      />
    );
  }

  return null;
};

type WadTabContentProps = {
  wadId: string;
  parentItemId?: string;
  items: WadItem[];
  pathComponents: WadItemPathComponent[];
};

const WadTabContent: React.FC<WadTabContentProps> = ({
  wadId,
  parentItemId,
  items,
  pathComponents,
}) => {
  const navigate = useNavigate();

  const [currentPreviewItemId, setCurrentPreviewItemId] = useState<
    string | null
  >(null);

  const wadState = useMemo<WadContextState>(
    () => ({
      wadId,
      currentPreviewItemId,
      changeCurrentPreviewItemId: (item) => setCurrentPreviewItemId(item),
      navigate: (item) => {
        if (item) {
          navigate(
            composeUrlQuery(generatePath(appRoutes.mountedWad, { wadId }), {
              itemId: item,
            }),
          );
        } else {
          navigate(generatePath(appRoutes.mountedWad, { wadId }));
        }
      },
    }),
    [currentPreviewItemId, navigate, wadId],
  );

  return (
    <WadContext.Provider value={wadState}>
      <div className="flex h-full flex-col gap-2">
        <div className="flex h-full flex-col rounded border border-gray-600 bg-gray-900">
          <div className="flex w-full flex-row flex-wrap items-center gap-2 border-b border-gray-600 bg-gray-800 p-2">
            <ExtractAllButton wadId={wadId} />
            <WadBreadcrumbs
              className="min-w-[400px] flex-1"
              wadId={wadId}
              pathComponents={pathComponents}
            />
            <WadSearchInput wadId={wadId} />
          </div>

          <PanelGroup className="w-full" direction="horizontal">
            <Panel defaultSize={70} minSize={30}>
              <WadItemList parentItemId={parentItemId} data={items} />
            </Panel>
            <PanelResizeHandle className="w-[1px] bg-gray-600" />
            <Panel defaultSize={30} minSize={30}>
              <div className="h-full bg-gray-950"></div>
            </Panel>
          </PanelGroup>
        </div>
      </div>
    </WadContext.Provider>
  );
};
