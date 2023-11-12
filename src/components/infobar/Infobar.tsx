import { ActionIcon, Popover } from '..';
import { TableSyncIcon, ToolboxIcon } from '../../assets';
import { ToolboxContent } from '../../features/toolbox';
import { env } from '../../utils';

export const Infobar = () => {
  return (
    <div className="flex min-h-[32px] flex-row  border border-t border-gray-600 bg-gray-800">
      {env.DEV && (
        <Popover.Root>
          <Popover.Trigger asChild>
            <ActionIcon size="lg" variant="ghost" icon={ToolboxIcon} />
          </Popover.Trigger>
          <Popover.Content className="w-[300px]" side="top" sideOffset={8}>
            <ToolboxContent />
          </Popover.Content>
        </Popover.Root>
      )}
      <ActionIcon size="lg" variant="ghost" icon={TableSyncIcon} />
    </div>
  );
};
