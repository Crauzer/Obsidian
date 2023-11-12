import * as RadixTooltip from '@radix-ui/react-tooltip';

export type TooltipTriggerProps = RadixTooltip.TooltipTriggerProps;

export const TooltipTrigger: React.FC<TooltipTriggerProps> = ({ children, ...props }) => {
  return <RadixTooltip.Trigger {...props}>{children}</RadixTooltip.Trigger>;
};
