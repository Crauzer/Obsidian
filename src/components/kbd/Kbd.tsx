import clsx from 'clsx';

export type KbdProps = {
  className?: string;
  children?: React.ReactNode;
};

export const Kbd: React.FC<KbdProps> = ({ className, children }) => {
  return (
    <kbd
      className={clsx(
        className,
        'px-[6px] py-1 font-mono font-bold rounded text-xs bg-gray-700/50 text-gray-50/75',
        'border border-gray-600/50 border-b-[3px]',
      )}
    >
      {children}
    </kbd>
  );
};
