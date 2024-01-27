import clsx from 'clsx';
import React from 'react';

export type KbdProps = {
  className?: string;
  children?: React.ReactNode;
};

export const Kbd: React.FC<KbdProps> = ({ className, children }) => {
  return (
    <kbd
      className={clsx(
        className,
        'font-mono rounded bg-gray-700/50 px-[6px] py-1 text-center text-xs font-bold text-gray-50/75',
        'border border-b-[3px] border-gray-600/50',
      )}
    >
      {children}
    </kbd>
  );
};
