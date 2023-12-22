import RadixDialog from '@radix-ui/react-dialog';
import React from 'react';

import { Progress } from '../progress';

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
        <RadixDialog.Overlay className="fixed inset-0 animate-fadeIn bg-gray-900/25 transition-opacity" />
        <RadixDialog.Content className="flex w-[400px] flex-col gap-2 bg-transparent">
          <Progress value={progress} />
          <p className="text-lg font-bold text-gray-50">{message}</p>
        </RadixDialog.Content>
      </RadixDialog.Portal>
    </RadixDialog.Root>
  );
};
