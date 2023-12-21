import * as RadixNavigationMenu from '@radix-ui/react-navigation-menu';
import clsx from 'clsx';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { PiGearSixDuotone } from 'react-icons/pi';
import { useMatches } from 'react-router-dom';

import { Button, Icon } from '..';
import Logo from '../../assets/logo.png';
import { Infobar } from '../../features/infobar';
import { appRoutes } from '../../lib/router';

type LayoutProps = React.PropsWithChildren;

export const Layout: React.FC<LayoutProps> = ({ children }) => {
  const [t] = useTranslation('route');

  return (
    <div className="flex h-full flex-col">
      <div
        className={clsx(
          'flex-column flex h-20 w-full items-center gap-2 px-4',
          'border-b border-b-obsidian-700/75',
          'bg-gradient-to-r from-obsidian-900/90 to-obsidian-700/60 backdrop-blur',
        )}
      >
        <a className="rounded-md" href="https://github.com/Crauzer/Obsidian" target="_blank">
          <img className="rounded-xl shadow-xl" width={60} height={60} src={Logo} />
        </a>

        <NavigationMenu
          items={[
            { title: t('explorer.title'), href: appRoutes.explorer },
            { title: t('mountedWads.title'), href: appRoutes.mountedWads },
          ]}
        />
        <div className="flex flex-1" />
      </div>
      <div className="flex w-full flex-1">{children}</div>
      <Infobar />
    </div>
  );
};

type NavigationMenuProps = { items: NavigationMenuItemProps[] };

const NavigationMenu: React.FC<NavigationMenuProps> = ({ items }) => {
  return (
    <RadixNavigationMenu.Root className="w-full">
      <RadixNavigationMenu.List className="center flex gap-2">
        {items.map((item) => (
          <NavigationMenuItem {...item} />
        ))}
        <NavigationMenuItem
          className="group ml-auto"
          title={
            <Icon
              size="xl"
              className="transition-transform duration-500 group-hover:rotate-90"
              icon={PiGearSixDuotone}
            />
          }
          href={appRoutes.settings}
        />
      </RadixNavigationMenu.List>
    </RadixNavigationMenu.Root>
  );
};

type NavigationMenuItemProps = {
  title: React.ReactNode;
  href: string;

  className?: string;
};

const NavigationMenuItem: React.FC<NavigationMenuItemProps> = ({ title, href, className }) => {
  const matches = useMatches();

  const match = matches.find((match) => match.pathname === href);

  return (
    <RadixNavigationMenu.Item className={className}>
      <RadixNavigationMenu.Link asChild>
        <Button
          className="text-xl font-bold text-gray-300"
          as="a"
          href={href}
          variant={match ? 'light' : 'ghost'}
        >
          {title}
        </Button>
      </RadixNavigationMenu.Link>
    </RadixNavigationMenu.Item>
  );
};
