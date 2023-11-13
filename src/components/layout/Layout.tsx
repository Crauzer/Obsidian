import * as RadixNavigationMenu from '@radix-ui/react-navigation-menu';
import clsx from 'clsx';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { useMatches, useNavigate } from 'react-router-dom';

import { Button, Infobar, Kbd } from '..';
import { CaretDownIcon, PlusRegularIcon } from '../../assets';
import Logo from '../../assets/logo.png';
import { useMountWads } from '../../features/wad';
import { appRoutes } from '../../lib/router';
import { composeUrlQuery } from '../../utils';
import { Menu } from '../menu';

type LayoutProps = React.PropsWithChildren;

export const Layout: React.FC<LayoutProps> = ({ children }) => {
  const [t] = useTranslation('route');
  const navigate = useNavigate();

  const mountWadMutation = useMountWads();

  const handleMountWads = () => {
    mountWadMutation.mutate(undefined, {
      onSuccess: ({ wadIds }) => {
        if (wadIds.length > 0) {
          navigate(composeUrlQuery(appRoutes.mountedWads, { wadId: wadIds[0] }));
        }
      },
    });
  };

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
        <Menu.Root modal={false}>
          <Menu.Trigger asChild>
            <Button compact variant="default" className="h-10 text-xl">
              <PlusRegularIcon width={16} height={16} />
              <CaretDownIcon width={16} height={16} />
            </Button>
          </Menu.Trigger>
          <Menu.Content align="start" sideOffset={6}>
            <Menu.Item className="flex flex-row p-1" onSelect={handleMountWads}>
              Mount Wads{' '}
              <span className="ml-auto text-sm text-gray-50">
                <Kbd>Ctrl + O</Kbd>
              </span>
            </Menu.Item>
          </Menu.Content>
        </Menu.Root>
      </div>
      <div className="flex w-full flex-1">{children}</div>
      <Infobar />
    </div>
  );
};

type NavigationMenuProps = { items: NavigationMenuItemProps[] };

const NavigationMenu: React.FC<NavigationMenuProps> = ({ items }) => {
  return (
    <RadixNavigationMenu.Root>
      <RadixNavigationMenu.List className="center flex gap-2">
        {items.map((item) => (
          <NavigationMenuItem {...item} />
        ))}
      </RadixNavigationMenu.List>
    </RadixNavigationMenu.Root>
  );
};

type NavigationMenuItemProps = {
  title: string;
  href: string;
};

const NavigationMenuItem: React.FC<NavigationMenuItemProps> = ({ title, href }) => {
  const matches = useMatches();

  const match = matches.find((match) => match.pathname === href);

  return (
    <RadixNavigationMenu.Item>
      <RadixNavigationMenu.Link asChild>
        <Button
          className="text-xl font-bold text-gray-300"
          as="a"
          href={href}
          variant={!!match ? 'light' : 'ghost'}
        >
          {title}
        </Button>
      </RadixNavigationMenu.Link>
    </RadixNavigationMenu.Item>
  );
};
