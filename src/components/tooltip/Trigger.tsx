import * as RadixTooltip from '@radix-ui/react-tooltip';
import React from 'react';

export type TooltipTriggerProps = RadixTooltip.TooltipTriggerProps;

export const TooltipTrigger: React.FC<TooltipTriggerProps> = ({ children, ...props }) => {
  return <RadixTooltip.Trigger {...props}>{children}</RadixTooltip.Trigger>;
};
