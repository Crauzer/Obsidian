import * as RadixTooltip from '@radix-ui/react-tooltip';
import clsx from 'clsx';
import React from 'react';

export type TooltipContentProps = RadixTooltip.TooltipContentProps;

export const TooltipContent: React.FC<TooltipContentProps> = ({
  children,
  className,
  sideOffset = 6,
  ...props
}) => {
  return (
    <RadixTooltip.Portal>
      <RadixTooltip.Content
        {...props}
        sideOffset={sideOffset}
        collisionPadding={8}
        className={clsx(
          'rounded border border-gray-600/50 bg-gray-800/50 px-2 py-1 text-sm text-gray-50 shadow-xl backdrop-blur',
          className,
        )}
      >
        {children}
        <RadixTooltip.Arrow className="fill-gray-800/90 backdrop-blur" />
      </RadixTooltip.Content>
    </RadixTooltip.Portal>
  );
};
