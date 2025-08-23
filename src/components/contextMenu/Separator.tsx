import * as RadixContextMenu from '@radix-ui/react-context-menu';
import clsx from 'clsx';
import React from 'react';

export const ContextMenuSeparator = React.forwardRef<
  React.ElementRef<typeof RadixContextMenu.Separator>,
  React.ComponentPropsWithoutRef<typeof RadixContextMenu.Separator>
>(({ className, ...props }, ref) => {
  return (
    <RadixContextMenu.Separator
      ref={ref}
      className={clsx('-mx-1 my-1 h-[1px] bg-gray-500', className)}
      {...props}
    />
  );
});

ContextMenuSeparator.displayName = 'ContextMenuSeparator';
