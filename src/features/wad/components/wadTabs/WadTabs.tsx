import * as RadixTabs from '@radix-ui/react-tabs';
import clsx from 'clsx';

import { CloseIcon } from '../../../../assets';
import { ActionIcon } from '../../../../components';
import { useMountedWads, useUnmountWad } from '../../api';
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
  const mountedWadsQuery = useMountedWads();

  const unmountWadMutation = useUnmountWad();

  const handleTabClose = (wadId: string) => {
    unmountWadMutation.mutate({ wadId });
  };

  if (mountedWadsQuery.isSuccess) {
    return (
      <RadixTabs.Root
        className="flex w-full flex-col"
        orientation="horizontal"
        value={selectedWad}
        onValueChange={onSelectedWadChanged}
      >
        <RadixTabs.List
          className={clsx(
            'flex data-[orientation=horizontal]:flex-row data-[orientation=vertical]:flex-col',
            'rounded-b-lg border-x border-b-2 border-gray-700 bg-gray-800',
          )}
        >
          {mountedWadsQuery.data.wads.map((mountedWad) => {
            return (
              <RadixTabs.Trigger
                key={mountedWad.id}
                value={mountedWad.id}
                className={clsx(
                  'group flex flex-row items-center justify-center gap-1 rounded-t-sm border-r border-r-gray-600 px-[0.5rem] py-[0.5rem] text-sm  text-gray-300',
                  'data-[state=active]:border-t data-[state=active]:border-t-obsidian-700 data-[state=active]:bg-gray-700',
                )}
              >
                {mountedWad.name}
                <ActionIcon
                  className="invisible ml-auto opacity-0 transition-opacity duration-150 group-hover:visible group-hover:opacity-100"
                  variant="ghost"
                  icon={CloseIcon}
                  onClick={() => handleTabClose(mountedWad.id)}
                />
              </RadixTabs.Trigger>
            );
          })}
        </RadixTabs.List>
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
    );
  }

  return null;
};
