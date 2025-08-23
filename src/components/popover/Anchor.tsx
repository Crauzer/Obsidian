import * as RadixPopover from "@radix-ui/react-popover";
import React, { forwardRef } from "react";

export type PopoverAnchorProps = RadixPopover.PopoverAnchorProps;

export const PopoverAnchor = forwardRef<HTMLDivElement, PopoverAnchorProps>(
  (props, ref) => {
    return <RadixPopover.Anchor {...props} ref={ref} />;
  },
);
