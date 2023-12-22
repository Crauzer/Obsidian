import RadixProgress from '@radix-ui/react-progress';
import clsx from 'clsx';
import React from 'react';

export const Progress: React.FC<RadixProgress.ProgressProps> = ({ className, value, ...props }) => {
  return (
    <RadixProgress.Progress
      {...props}
      value={value}
      className={clsx('relative h-4 overflow-hidden rounded-lg bg-gray-900/50', className)}
    >
      <RadixProgress.Indicator
        className="h-full w-full"
        style={{ transform: `translateX(-${100 - (value ?? 0)}%)` }}
      />
    </RadixProgress.Progress>
  );
};
