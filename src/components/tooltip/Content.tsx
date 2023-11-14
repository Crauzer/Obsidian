import * as RadixTooltip from '@radix-ui/react-tooltip';
import clsx from 'clsx';

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
        className={clsx(
          className,
          'rounded border border-gray-600/50 bg-gray-800/80 px-2 py-1 text-gray-50 shadow-xl',
          'origin-[var(--radix-popover-content-transform-origin)]',
        )}
      >
        {children}
        <RadixTooltip.Arrow className="fill-gray-800/80" />
      </RadixTooltip.Content>
    </RadixTooltip.Portal>
  );
};
