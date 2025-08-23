import RadixDialog from "@radix-ui/react-dialog";
import type React from "react";

export type DialogContentProps = RadixDialog.DialogContentProps;

export const DialogContent: React.FC<DialogContentProps> = (props) => {
  return (
    <RadixDialog.Portal>
      <RadixDialog.Overlay />
      <RadixDialog.Content {...props} />
    </RadixDialog.Portal>
  );
};
