import * as RadixContextMenu from '@radix-ui/react-context-menu';
import clsx from 'clsx';
import React from 'react';
import { PiCaretRight, PiCheckBold, PiCircleFill } from 'react-icons/pi';

import { Icon } from '../icon';
import { ContextMenuContent } from './Content';
import { ContextMenuItem } from './Item';
import { ContextMenuRoot } from './Root';
import { ContextMenuSeparator } from './Separator';
import { ContextMenuTrigger } from './Trigger';

export * from './Root';
export * from './Trigger';
export * from './Content';
export * from './Item';
export * from './Separator';

const ContextMenuGroup = RadixContextMenu.Group;

const ContextMenuSub = RadixContextMenu.Sub;

const ContextMenuRadioGroup = RadixContextMenu.RadioGroup;

const ContextMenuPortal = RadixContextMenu.Portal;

const ContextMenuSubTrigger = React.forwardRef<
  React.ElementRef<typeof RadixContextMenu.SubTrigger>,
  React.ComponentPropsWithoutRef<typeof RadixContextMenu.SubTrigger> & {
    inset?: boolean;
  }
>(({ className, inset, children, ...props }, ref) => (
  <RadixContextMenu.SubTrigger
    ref={ref}
    className={clsx(
      'focus:bg-accent focus:text-accent-foreground data-[state=open]:bg-accent data-[state=open]:text-accent-foreground flex cursor-default select-none items-center rounded-sm px-2 py-1.5 text-sm outline-none',
      inset && 'pl-8',
      className,
    )}
    {...props}
  >
    {children}
    <Icon icon={PiCaretRight} className="ml-auto h-4 w-4" />
  </RadixContextMenu.SubTrigger>
));
ContextMenuSubTrigger.displayName = RadixContextMenu.SubTrigger.displayName;

const ContextMenuSubContent = React.forwardRef<
  React.ElementRef<typeof RadixContextMenu.SubContent>,
  React.ComponentPropsWithoutRef<typeof RadixContextMenu.SubContent>
>(({ className, ...props }, ref) => (
  <RadixContextMenu.SubContent
    ref={ref}
    className={clsx(
      className,
      'flex min-w-[250px] flex-col gap-y-1 rounded-lg border border-gray-500 bg-gray-700/75 p-2 shadow-xl backdrop-blur',
      'will-change-[opacity,transform]',
      'max-h-[--radix-context-menu-content-available-height] overflow-y-auto overflow-x-hidden',

      'data-[state=open]:data-[side=top]:animate-slideAndFadeInFromTop',
      'data-[state=open]:data-[side=right]:animate-slideAndFadeInFromRight',
      'data-[state=open]:data-[side=bottom]:animate-slideAndFadeInFromBottom',
      'data-[state=open]:data-[side=left]:animate-slideAndFadeInFromLeft',

      'data-[state=closed]:data-[side=top]:animate-slideAndFadeOutFromTop',
      'data-[state=closed]:data-[side=right]:animate-slideAndFadeOutFromRight',
      'data-[state=closed]:data-[side=bottom]:animate-slideAndFadeOutFromBottom',
      'data-[state=closed]:data-[side=left]:animate-slideAndFadeOutFromLeft',

      'origin-[--radix-context-menu-content-transform-origin]',
    )}
    {...props}
  />
));
ContextMenuSubContent.displayName = RadixContextMenu.SubContent.displayName;

const ContextMenuCheckboxItem = React.forwardRef<
  React.ElementRef<typeof RadixContextMenu.CheckboxItem>,
  React.ComponentPropsWithoutRef<typeof RadixContextMenu.CheckboxItem>
>(({ className, children, checked, ...props }, ref) => (
  <RadixContextMenu.CheckboxItem
    ref={ref}
    className={clsx(
      'focus:bg-accent focus:text-accent-foreground relative flex cursor-default select-none items-center rounded-sm py-1.5 pl-8 pr-2 text-sm outline-none data-[disabled]:pointer-events-none data-[disabled]:opacity-50',
      className,
    )}
    checked={checked}
    {...props}
  >
    <span className="absolute left-2 flex h-3.5 w-3.5 items-center justify-center">
      <RadixContextMenu.ItemIndicator>
        <Icon icon={PiCheckBold} size="sm" />
      </RadixContextMenu.ItemIndicator>
    </span>
    {children}
  </RadixContextMenu.CheckboxItem>
));
ContextMenuCheckboxItem.displayName = RadixContextMenu.CheckboxItem.displayName;

const ContextMenuRadioItem = React.forwardRef<
  React.ElementRef<typeof RadixContextMenu.RadioItem>,
  React.ComponentPropsWithoutRef<typeof RadixContextMenu.RadioItem>
>(({ className, children, ...props }, ref) => (
  <RadixContextMenu.RadioItem
    ref={ref}
    className={clsx(
      'focus:bg-accent focus:text-accent-foreground relative flex cursor-default select-none items-center rounded-sm py-1.5 pl-8 pr-2 text-sm outline-none data-[disabled]:pointer-events-none data-[disabled]:opacity-50',
      className,
    )}
    {...props}
  >
    <span className="absolute left-2 flex h-3.5 w-3.5 items-center justify-center">
      <RadixContextMenu.ItemIndicator>
        <Icon icon={PiCircleFill} size="sm" />
      </RadixContextMenu.ItemIndicator>
    </span>
    {children}
  </RadixContextMenu.RadioItem>
));
ContextMenuRadioItem.displayName = RadixContextMenu.RadioItem.displayName;

const ContextMenuLabel = React.forwardRef<
  React.ElementRef<typeof RadixContextMenu.Label>,
  React.ComponentPropsWithoutRef<typeof RadixContextMenu.Label> & {
    inset?: boolean;
  }
>(({ className, inset, ...props }, ref) => (
  <RadixContextMenu.Label
    ref={ref}
    className={clsx(
      'text-foreground px-2 py-1.5 text-sm font-semibold',
      inset && 'pl-8',
      className,
    )}
    {...props}
  />
));
ContextMenuLabel.displayName = RadixContextMenu.Label.displayName;

const ContextMenuShortcut = ({ className, ...props }: React.HTMLAttributes<HTMLSpanElement>) => {
  return (
    <span
      className={clsx('text-muted-foreground ml-auto text-xs tracking-widest', className)}
      {...props}
    />
  );
};
ContextMenuShortcut.displayName = 'ContextMenuShortcut';

export const ContextMenu = {
  Root: ContextMenuRoot,
  Trigger: ContextMenuTrigger,
  Content: ContextMenuContent,
  Item: ContextMenuItem,
  Separator: ContextMenuSeparator,
  Group: ContextMenuGroup,
  Sub: ContextMenuSub,
  SubTrigger: ContextMenuSubTrigger,
  SubContent: ContextMenuSubContent,
  CheckboxItem: ContextMenuCheckboxItem,
  RadioItem: ContextMenuRadioItem,
  Portal: ContextMenuPortal,
};
