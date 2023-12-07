import {
  DragDropContext,
  Draggable,
  DraggableProvided,
  DraggableProvidedDragHandleProps,
  DraggableStateSnapshot,
  Droppable,
  OnDragEndResponder,
} from '@hello-pangea/dnd';
import * as RadixTabs from '@radix-ui/react-tabs';
import clsx from 'clsx';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { LuPackagePlus } from 'react-icons/lu';
import { RxDragHandleDots2 } from 'react-icons/rx';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';

import { CloseIcon } from '../../../../assets';
import { Button, Icon, Tooltip } from '../../../../components';
import { appRoutes } from '../../../../lib/router';
import { composeUrlQuery } from '../../../../utils';
import { useMountWads, useMountedWads, useReorderMountedWad, useUnmountWad } from '../../api';
import { MountedWad } from '../../types';
import { WadDirectoryTabContent, WadTabContent } from './WadTabContent';

export type WadTabsProps = {
  selectedWad?: string;
  selectedItemId?: string;
  onSelectedWadChanged?: (selectedWad: string) => void;
};

export const WadTabs: React.FC<WadTabsProps> = ({
  selectedWad,
  selectedItemId,
  onSelectedWadChanged,
}) => {
  const [t] = useTranslation('mountedWads');
  const navigate = useNavigate();

  const mountedWadsQuery = useMountedWads();

  const mountWadsMutation = useMountWads();
  const unmountWadMutation = useUnmountWad();
  const reorderWadMutation = useReorderMountedWad();

  const handleTabClose = (wadId: string) => {
    unmountWadMutation.mutate({ wadId });
  };

  const handleDragEnd: OnDragEndResponder = (result, _provided) => {
    if (result.destination) {
      reorderWadMutation.mutate({
        sourceIndex: result.source.index,
        destIndex: result.destination.index,
      });
    }
  };

  const handleMountWads = () => {
    mountWadsMutation.mutate(undefined, {
      onSuccess: ({ wadIds }) => {
        if (wadIds.length === 0) {
          return;
        }

        toast.success(t('mountSuccess', { count: wadIds.length }));

        navigate(composeUrlQuery(appRoutes.mountedWads, { wadId: wadIds[0] }));
      },
    });
  };

  if (mountedWadsQuery.isSuccess) {
    return (
      <DragDropContext onDragEnd={handleDragEnd}>
        <RadixTabs.Root
          className="flex w-full flex-col gap-2"
          orientation="horizontal"
          value={selectedWad}
          onValueChange={onSelectedWadChanged}
        >
          <div className="flex w-full flex-row">
            <Droppable droppableId="wad_tabs" direction="horizontal">
              {(provided, snapshot) => (
                <RadixTabs.List
                  ref={provided.innerRef}
                  {...provided.droppableProps}
                  className={clsx(
                    'flex flex-1',
                    'data-[orientation=horizontal]:flex-row data-[orientation=vertical]:flex-col',
                    'rounded rounded-r-none border border-gray-700 bg-gray-800 transition-colors',
                    'overflow-x-scroll [scrollbar-gutter:stable]',
                    'relative min-h-[2.5rem]',
                    { 'border-obsidian-500 ': snapshot.isDraggingOver },
                  )}
                >
                  {mountedWadsQuery.data.wads.map((mountedWad, index) => {
                    return (
                      <Draggable key={mountedWad.id} draggableId={mountedWad.id} index={index}>
                        {(provided, snapshot) => (
                          <TabTrigger
                            key={mountedWad.id}
                            mountedWad={mountedWad}
                            provided={provided}
                            snapshot={snapshot}
                            handleTabClose={handleTabClose}
                          />
                        )}
                      </Draggable>
                    );
                  })}
                </RadixTabs.List>
              )}
            </Droppable>
            <Tooltip.Root>
              <Tooltip.Trigger asChild>
                <Button
                  variant="filled"
                  className="h-full rounded-l-none text-xl"
                  onClick={() => handleMountWads()}
                >
                  <Icon size="xl" icon={LuPackagePlus} />
                </Button>
              </Tooltip.Trigger>
              <Tooltip.Content side="bottom">Mount Wads</Tooltip.Content>
            </Tooltip.Root>
          </div>
          {mountedWadsQuery.data.wads.map((mountedWad) => {
            return (
              <RadixTabs.Content key={mountedWad.id} className="flex-1" value={mountedWad.id}>
                {selectedItemId ? (
                  <WadDirectoryTabContent wadId={mountedWad.id} selectedItemId={selectedItemId} />
                ) : (
                  <WadTabContent wadId={mountedWad.id} />
                )}
              </RadixTabs.Content>
            );
          })}
        </RadixTabs.Root>
      </DragDropContext>
    );
  }

  return null;
};

type TabTriggerProps = {
  mountedWad: MountedWad;

  provided: DraggableProvided;
  snapshot: DraggableStateSnapshot;
  handleTabClose: (wadId: string) => void;
};

const TabTrigger: React.FC<TabTriggerProps> = ({
  mountedWad,
  handleTabClose,
  provided,
  snapshot,
}) => {
  const [t] = useTranslation('mountedWads');

  return (
    <Tooltip.Root>
      <Tooltip.Trigger asChild>
        <div ref={provided.innerRef} {...provided.draggableProps}>
          <RadixTabs.Trigger
            value={mountedWad.id}
            className={clsx(
              'group flex h-full flex-row items-center justify-center gap-1 rounded-t-sm border-r border-r-gray-600 bg-gray-800 px-[0.5rem] py-[0.25rem] text-sm hover:bg-gray-700',
              'data-[state=active]:border-t-2 data-[state=active]:border-t-obsidian-700 data-[state=active]:bg-gray-700',
              { 'border-t border-t-obsidian-700 ': snapshot.isDragging },
            )}
          >
            <TabTriggerDragHandle dragHandleProps={provided.dragHandleProps} />
            {mountedWad.name}
            <TabTriggerCloseButton onClick={() => handleTabClose(mountedWad.id)} />
          </RadixTabs.Trigger>
        </div>
      </Tooltip.Trigger>
      <Tooltip.Content side="bottom" className="text-xs">
        {mountedWad.wadPath}
      </Tooltip.Content>
    </Tooltip.Root>
  );
};

type TabTriggerDragHandleProps = {
  dragHandleProps: DraggableProvidedDragHandleProps | null;
};

const TabTriggerDragHandle: React.FC<TabTriggerDragHandleProps> = ({ dragHandleProps }) => {
  const [t] = useTranslation('mountedWads');

  return (
    <Tooltip.Root>
      <Tooltip.Trigger asChild>
        <div
          {...dragHandleProps}
          className="flex items-center justify-center rounded transition-colors"
        >
          <Icon size="lg" className="text-gray-400" icon={RxDragHandleDots2} />
        </div>
      </Tooltip.Trigger>
      <Tooltip.Content className="text-sm">{t('tab.dndTooltip')}</Tooltip.Content>
    </Tooltip.Root>
  );
};

type TabTriggerCloseButtonProps = {
  onClick: React.MouseEventHandler<SVGSVGElement>;
};

const TabTriggerCloseButton: React.FC<TabTriggerCloseButtonProps> = ({ onClick }) => {
  const [t] = useTranslation('mountedWads');

  return (
    <Tooltip.Root>
      <Tooltip.Trigger asChild>
        <span className="invisible ml-auto rounded p-1 opacity-0 transition-opacity duration-150 hover:bg-obsidian-500/40 group-hover:visible group-hover:opacity-100">
          <Icon size="md" icon={CloseIcon} onClick={onClick} />
        </span>
      </Tooltip.Trigger>
      <Tooltip.Content side="top" className="text-sm">
        {t('tab.closeTooltip')}
      </Tooltip.Content>
    </Tooltip.Root>
  );
};
