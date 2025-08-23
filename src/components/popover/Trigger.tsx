import * as RadixPopover from "@radix-ui/react-popover";
import type React from "react";

export type PopoverTriggerProps = RadixPopover.PopoverTriggerProps;

export const PopoverTrigger: React.FC<PopoverTriggerProps> = (props) => {
  return (
    <RadixPopover.Trigger {...props} asChild>
      {props.children}
    </RadixPopover.Trigger>
  );
};
