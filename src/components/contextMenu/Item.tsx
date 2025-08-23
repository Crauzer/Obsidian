import * as RadixContextMenu from '@radix-ui/react-context-menu';
import clsx from 'clsx';
import React from 'react';

export const ContextMenuItem = React.forwardRef<
  React.ElementRef<typeof RadixContextMenu.Item>,
  React.ComponentPropsWithoutRef<typeof RadixContextMenu.Item>
>((props, ref) => {
  return (
    <RadixContextMenu.Item
      {...props}
      ref={ref}
      className={clsx(
        props.className,
        'text-md rounded border-none px-2 py-[1px] text-gray-50 transition-colors',
        'data-[disabled]:cursor-not-allowed  data-[disabled]:text-gray-500',
        'data-[highlighted]:bg-gray-500/20',
        'cursor-pointer select-none outline-none',
      )}
    />
  );
});

ContextMenuItem.displayName = RadixContextMenu.Item.displayName;
