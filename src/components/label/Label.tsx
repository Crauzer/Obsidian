import * as RadixLabel from "@radix-ui/react-label";
import type React from "react";

export type LabelProps = RadixLabel.LabelProps;

export const Label: React.FC<LabelProps> = (props) => {
  return <RadixLabel.Root {...props}>{props.children}:</RadixLabel.Root>;
};
