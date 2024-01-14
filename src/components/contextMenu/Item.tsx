import * as RadixContextMenu from '@radix-ui/react-context-menu';
import clsx from 'clsx';
import React from 'react';

export type ContextMenuItemProps = RadixContextMenu.MenuItemProps;

export const ContextMenuItem: React.FC<ContextMenuItemProps> = (props) => {
  return (
    <RadixContextMenu.Item
      {...props}
      className={clsx(
        props.className,
        'text-md rounded border-none px-2 py-[1px] text-gray-50 transition-colors',
        'data-[highlighted]:bg-gray-500/20',
        'cursor-pointer select-none outline-none',
      )}
    />
  );
};
