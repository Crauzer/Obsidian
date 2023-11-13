import {
  DragDropContext,
  Draggable,
  DraggableProvided,
  DraggableStateSnapshot,
  Droppable,
  OnDragEndResponder,
} from '@hello-pangea/dnd';
import * as RadixTabs from '@radix-ui/react-tabs';
import clsx from 'clsx';
import { CSSProperties } from 'react';
import { BiDotsVertical } from 'react-icons/bi';
import { useNavigate } from 'react-router-dom';

import { CaretDownIcon, CloseIcon, PlusRegularIcon } from '../../../../assets';
import { ActionIcon, Button, Icon, Kbd, Menu } from '../../../../components';
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
        if (wadIds.length > 0) {
          navigate(composeUrlQuery(appRoutes.mountedWads, { wadId: wadIds[0] }));
        }
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
          <div className="flex w-full flex-row gap-2">
            <Droppable droppableId="wad_tabs" direction="horizontal">
              {(provided, snapshot) => (
                <RadixTabs.List
                  ref={provided.innerRef}
                  {...provided.droppableProps}
                  className={clsx(
                    'flex flex-1 transition-colors data-[orientation=horizontal]:flex-row data-[orientation=vertical]:flex-col',
                    'rounded border border-gray-700 bg-gray-800',
                    'overflow-x-scroll [scrollbar-gutter:stable_]',
                    'relative',
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
            <Menu.Root modal={false}>
              <Menu.Trigger asChild>
                <Button compact variant="default" className="h-10 text-xl">
                  <PlusRegularIcon width={16} height={16} />
                  <CaretDownIcon width={16} height={16} />
                </Button>
              </Menu.Trigger>
              <Menu.Content align="start" sideOffset={6}>
                <Menu.Item className="flex flex-row p-1" onSelect={handleMountWads}>
                  Mount Wads{' '}
                  <span className="ml-auto text-sm text-gray-50">
                    <Kbd>Ctrl + O</Kbd>
                  </span>
                </Menu.Item>
              </Menu.Content>
            </Menu.Root>
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
  return (
    <div ref={provided.innerRef} {...provided.draggableProps}>
      <RadixTabs.Trigger
        value={mountedWad.id}
        className={clsx(
          'group flex h-full flex-row items-center justify-center gap-1 rounded-t-sm border-r border-r-gray-600 bg-gray-800 px-[0.5rem] py-[0.25rem] text-sm text-gray-300 hover:bg-gray-700',
          'data-[state=active]:border-t-2 data-[state=active]:border-t-obsidian-700 data-[state=active]:bg-gray-700',
          { 'border-t border-t-obsidian-700 ': snapshot.isDragging },
        )}
      >
        <div
          {...provided.dragHandleProps}
          className="flex items-center justify-center rounded transition-colors hover:bg-obsidian-500/30"
        >
          <Icon size="lg" icon={BiDotsVertical} />
        </div>
        {mountedWad.name}
        <span className="invisible ml-auto rounded p-1 opacity-0 transition-opacity duration-150 hover:bg-obsidian-500/40 group-hover:visible group-hover:opacity-100">
          <Icon size="md" icon={CloseIcon} onClick={() => handleTabClose(mountedWad.id)} />
        </span>
      </RadixTabs.Trigger>
    </div>
  );
};
