import clsx from 'clsx';

export type BreadcrumbsRootProps = {
  className?: string;
  children?: React.ReactNode;
};

export const BreadcrumbsRoot: React.FC<BreadcrumbsRootProps> = ({ className, children }) => {
  return <div className={clsx(className, 'flex flex-row gap-1')}>{children}</div>;
};
