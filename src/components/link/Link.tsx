import clsx from 'clsx';
import React from 'react';

export type LinkProps = React.DetailedHTMLProps<
  React.AnchorHTMLAttributes<HTMLAnchorElement>,
  HTMLAnchorElement
>;

export const Link: React.FC<LinkProps> = ({ className, ...props }) => {
  return (
    <a
      {...props}
      className={clsx(
        'font-bold text-obsidian-500 transition-colors hover:text-obsidian-600 hover:underline',
        className,
      )}
    >
      {props.children}
    </a>
  );
};
