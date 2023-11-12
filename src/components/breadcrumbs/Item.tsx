import clsx from 'clsx';

export type BreadcrumbItemProps = {
  title?: React.ReactNode;
  href: string;

  className?: string;
};

export const BreadcrumbItem: React.FC<BreadcrumbItemProps> = ({ title, href, className }) => {
  return (
    <a
      className={clsx(
        className,
        'text-md flex items-center text-gray-50 transition-colors hover:text-obsidian-500 hover:underline',
      )}
      href={href}
    >
      {title}
    </a>
  );
};
