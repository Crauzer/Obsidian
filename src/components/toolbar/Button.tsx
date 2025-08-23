import * as RadixToolbar from "@radix-ui/react-toolbar";
import type React from "react";

export type ToolbarButtonProps = RadixToolbar.ToolbarButtonProps;

export const ToolbarButton: React.FC<ToolbarButtonProps> = ({ ...props }) => {
  return <RadixToolbar.Button {...props} />;
};
