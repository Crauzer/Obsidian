import clsx from 'clsx';
import { forwardRef } from 'react';

export type BreadcrumbItemProps = {
  href: string;

  className?: string;
} & React.DetailedHTMLProps<React.AnchorHTMLAttributes<HTMLAnchorElement>, HTMLAnchorElement>;

export const BreadcrumbItem = forwardRef<HTMLAnchorElement, BreadcrumbItemProps>(
  ({ content, href, className, children, ...props }, ref) => {
    return (
      <a
        {...props}
        ref={ref}
        className={clsx(
          className,
          'text-md flex items-center text-gray-50 transition-colors hover:text-obsidian-500 hover:underline',
        )}
        href={href}
      >
        {children}
      </a>
    );
  },
);
