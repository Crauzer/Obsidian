import * as RadixPopover from '@radix-ui/react-popover';
import clsx from 'clsx';
import React, { forwardRef } from 'react';

export type PopoverContentProps = RadixPopover.PopoverContentProps;

export const PopoverContent = forwardRef<HTMLDivElement, PopoverContentProps>(
  ({ children, className, sideOffset = 12, ...props }, ref) => {
    return (
      <RadixPopover.Portal>
        <div className="shadow-xl ">
          <RadixPopover.Content
            {...props}
            ref={ref}
            sideOffset={sideOffset}
            // TODO: This is a hack https://github.com/radix-ui/primitives/issues/2248
            onOpenAutoFocus={(e) => e.preventDefault()}
            className={clsx(
              className,
              'rounded border border-gray-500 bg-gray-700/75 p-2 shadow-inner backdrop-blur',
              'origin-[var(--radix-popover-content-transform-origin)] will-change-[transform,opacity]',

              'data-[state=open]:data-[side=top]:animate-slideAndFadeInFromTop',
              'data-[state=open]:data-[side=right]:animate-slideAndFadeInFromRight',
              'data-[state=open]:data-[side=bottom]:animate-slideAndFadeInFromBottom',
              'data-[state=open]:data-[side=left]:animate-slideAndFadeInFromLeft',

              'data-[state=closed]:data-[side=top]:animate-slideAndFadeOutFromTop',
              'data-[state=closed]:data-[side=right]:animate-slideAndFadeOutFromRight',
              'data-[state=closed]:data-[side=bottom]:animate-slideAndFadeOutFromBottom',
              'data-[state=closed]:data-[side=left]:animate-slideAndFadeOutFromLeft',
            )}
          >
            {children}
          </RadixPopover.Content>
        </div>
      </RadixPopover.Portal>
    );
  },
);
