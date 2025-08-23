import * as RadixDialog from "@radix-ui/react-dialog";
import type React from "react";

import { Progress } from "../progress";

export type LoadingOverlayProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;

  progress: number;
  message?: React.ReactNode;
};

export const LoadingOverlay: React.FC<LoadingOverlayProps> = ({
  open,
  onOpenChange,
  progress,
  message,
}) => {
  return (
    <RadixDialog.Root open={open} onOpenChange={onOpenChange}>
      <RadixDialog.Portal>
        <RadixDialog.Overlay className="fixed inset-0 animate-fadeIn bg-gray-900/40 transition-opacity" />
        <RadixDialog.Content className="fixed left-[50%] top-[50%] w-[60vw] translate-x-[-50%] translate-y-[-50%] transform bg-transparent">
          <div className="flex flex-col items-center gap-1">
            <Progress className="w-full" value={progress} />
            <p className="text-lg text-gray-50">{message}</p>
          </div>
        </RadixDialog.Content>
      </RadixDialog.Portal>
    </RadixDialog.Root>
  );
};
