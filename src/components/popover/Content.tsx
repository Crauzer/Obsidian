import * as RadixPopover from '@radix-ui/react-popover';
import clsx from 'clsx';

export type PopoverContentProps = RadixPopover.PopoverContentProps;

export const PopoverContent: React.FC<PopoverContentProps> = ({
  children,
  className,
  sideOffset = 12,
  ...props
}) => {
  return (
    <RadixPopover.Portal>
      <RadixPopover.Content
        {...props}
        sideOffset={sideOffset}
        // TODO: This is a hack https://github.com/radix-ui/primitives/issues/2248
        onOpenAutoFocus={(e) => e.preventDefault()}
        className={clsx(
          className,
          'p-4 bg-gray-800 rounded shadow-xl',
          'will-change-[transform,opacity] origin-[var(--radix-popover-content-transform-origin)]',

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
    </RadixPopover.Portal>
  );
};
