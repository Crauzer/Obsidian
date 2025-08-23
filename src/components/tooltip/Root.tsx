import * as RadixTooltip from "@radix-ui/react-tooltip";
import type React from "react";

export type TooltipProps = RadixTooltip.TooltipProps;

export const TooltipRoot: React.FC<TooltipProps> = ({ children, ...props }) => {
  return <RadixTooltip.Root {...props}>{children}</RadixTooltip.Root>;
};
