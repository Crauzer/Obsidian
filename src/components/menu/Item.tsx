import * as RadixMenu from '@radix-ui/react-dropdown-menu';
import clsx from 'clsx';
import React from 'react';

export type MenuItemProps = RadixMenu.DropdownMenuItemProps;

export const MenuItem: React.FC<MenuItemProps> = (props) => {
  return (
    <RadixMenu.Item
      {...props}
      className={clsx(
        props.className,
        'text-md rounded border-none px-2 text-gray-50 transition-colors',
        'data-[highlighted]:bg-gray-500/20',
        'cursor-pointer select-none outline-none',
      )}
    />
  );
};
