import clsx from 'clsx';
import React from 'react';

export const Skeleton = ({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) => {
  return <div className={clsx('animate-pulse rounded-md bg-gray-500/30', className)} {...props} />;
};
