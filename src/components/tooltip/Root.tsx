import * as RadixTooltip from '@radix-ui/react-tooltip';

export type TooltipProps = RadixTooltip.TooltipProps;

export const TooltipRoot: React.FC<TooltipProps> = ({ children, ...props }) => {
  return <RadixTooltip.Root {...props}>{children}</RadixTooltip.Root>;
};
