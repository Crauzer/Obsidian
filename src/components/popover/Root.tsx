import * as RadixPopover from '@radix-ui/react-popover';

export type PopoverProps = RadixPopover.PopoverProps;

export const PopoverRoot: React.FC<PopoverProps> = (props) => {
  return <RadixPopover.Root {...props}></RadixPopover.Root>;
};
