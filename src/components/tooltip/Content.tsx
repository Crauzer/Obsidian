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
          'py-1 px-2 bg-gray-800/50 rounded shadow-xl',
          'will-change-[opacity] origin-[var(--radix-popover-content-transform-origin)]',
          'data-[state=delayed-open]:data-[side=top]:animate-fadeIn data-[state=closed]:data-[side=top]:animate-fadeOut',
        )}
      >
        {children}
        <RadixTooltip.Arrow className="fill-gray-800" />
      </RadixTooltip.Content>
    </RadixTooltip.Portal>
  );
};
